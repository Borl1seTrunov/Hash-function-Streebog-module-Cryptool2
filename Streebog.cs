using CrypTool.PluginBase;
using CrypTool.PluginBase.Attributes;
using CrypTool.PluginBase.Miscellaneous;
using CrypTool.PluginBase.IO;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Streebog
{
    [Author("Open Source", "add0das46@gmail.com", "CrypTool 2", "https://github.com/CrypTool")]
    [PluginInfo(
        "Стрибог",
        "Вычисляет хэш-функцию ГОСТ Р 34.11-2012 (Стрибог) с размером хэша 256 или 512 бит. IV задаётся входом; если не подключён, используется стандартный.",
        "Streebog/DetailedDescription/doc.xml",
        new[] { "Streebog/icon.png" })]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    [ComponentVisualAppearance(ComponentVisualAppearance.VisualAppearanceEnum.Opened)]
    public class Streebog : DependencyObject, ICrypComponent
    {
        private readonly StreebogPresentation presentation;
        private ISettings settings;
        private byte[] lastDigest;

        public Streebog()
        {
            presentation = new StreebogPresentation();
            settings = new StreebogSettings();
        }

        [PropertyInfo(Direction.InputData, "Входные данные", "Данные для хэширования.", true)]
        public ICrypToolStream InputData
        {
            get { return inputData; }
            set { inputData = value; }
        }

        [PropertyInfo(Direction.InputData, "Вектор инициализации (IV)", "Необязательный поток с 64 байтами (512 бит) вектора инициализации. Если не подключён, используется стандартный IV для выбранного размера хэша (256 или 512 бит).", false)]
        public ICrypToolStream IVData
        {
            get { return ivData; }
            set { ivData = value; }
        }

        [PropertyInfo(Direction.OutputData, "Выходные данные", "Вычисленный хэш.", true)]
        public byte[] OutputData
        {
            get { return lastDigest; }
            set
            {
                if (!AreEqual(lastDigest, value))
                {
                    lastDigest = value;
                    OnPropertyChanged("OutputData");
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "Выходной поток", "Вычисленный хэш в виде потока.", true)]
        public ICrypToolStream OutputDataStream
        {
            get { return outputDataStream; }
            set
            {
                if (!ReferenceEquals(outputDataStream, value))
                {
                    outputDataStream = value;
                    OnPropertyChanged("OutputDataStream");
                }
            }
        }

        public ISettings Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        public UserControl Presentation
        {
            get { return presentation; }
        }

        private ICrypToolStream inputData;
        private ICrypToolStream ivData;
        private ICrypToolStream outputDataStream;

        #region Events required by IPlugin / ICrypComponent

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PropertyChangedEventHandler PropertyChanged;
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        #endregion

        #region ICrypComponent lifecycle

        public static byte[] ComputeHash(byte[] data, int hashSize)
        {
            return ComputeHash(data, hashSize, null);
        }

        public static byte[] ComputeHash(byte[] data, int hashSize, byte[] customIv)
        {
            StreebogCore core = new StreebogCore();
            core.Init(hashSize, customIv);
            core.Update(data, 0, data.Length);
            return core.Final();
        }

        public void Initialize()
        {
        }

        public void PreExecution()
        {
            lastDigest = null;
            outputDataStream = null;
            presentation.Reset();
        }

        public void Execute()
        {
            int hashSize = 512;
            StreebogSettings typedSettings = settings as StreebogSettings;
            if (typedSettings != null)
            {
                hashSize = typedSettings.HashSize == 0 ? 256 : 512;
            }

            byte[] input = ReadInputData();
            byte[] iv = ResolveIv();
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this,
                iv == null
                    ? "IV не подключён: используется стандартный IV."
                    : "IV подключён: используется пользовательский IV.");
            StreebogCore core = new StreebogCore();
            core.BlockCount = input.Length / 64 + 1;
            core.Trace = step =>
            {
                presentation.OnStep(step);
                if (presentation.IsSkip) core.SkipTrace = true;
                if (presentation.IsStopped) core.SkipTrace = true;
            };

            core.Init(hashSize, iv);
            core.Update(input, 0, input.Length);
            byte[] digest = core.Final();

            if (presentation.IsStopped)
            {
                return;
            }

            PublishOutput(digest);
            presentation.SetOutput(digest);

            Progress(1, 1);
        }

        public void PostExecution()
        {
            presentation.Stop();
        }

        public void Stop()
        {
            presentation.Stop();
        }

        public void Dispose()
        {
        }

        #endregion

        private byte[] ResolveIv()
        {
            if (ivData == null)
            {
                return null;
            }

            byte[] iv = ivData.CreateReader().ReadFully();
            if (iv.Length == 0)
            {
                return null;
            }

            if (iv.Length != 64)
            {
                throw new ArgumentException(
                    "Вектор инициализации IV должен содержать ровно 64 байта (512 бит), а содержит " + iv.Length + ".",
                    "IV");
            }

            return iv;
        }

        private byte[] ReadInputData()
        {
            if (inputData == null)
            {
                return new byte[0];
            }

            ICrypToolStream stream = inputData;
            CStreamReader reader = stream.CreateReader();
            return reader.ReadFully();
        }

        private void PublishOutput(byte[] digest)
        {
            lastDigest = digest;
            outputDataStream = new CStreamWriter(digest);

            OnPropertyChanged("OutputData");
            OnPropertyChanged("OutputDataStream");
        }

        private void Progress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(propertyName));
        }

        private static bool AreEqual(byte[] left, byte[] right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }
            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            for (int i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
