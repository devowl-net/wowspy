namespace WowSpy
{
    public partial class ResultWindow
    {
        public string Info { get; set; }

        public ResultWindow(string info)
        {
            DataContext = this;
            Info = info;
            InitializeComponent();
        }
    }
}
