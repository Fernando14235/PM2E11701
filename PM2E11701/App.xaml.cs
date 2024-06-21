namespace PM2E11701
{
    public partial class App : Application
    {

        public static Controllers.PlaceController PlaceController { get; private set; }
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
            PlaceController = new Controllers.PlaceController();
        }
    }
}
