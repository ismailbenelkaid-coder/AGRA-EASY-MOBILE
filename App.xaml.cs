namespace AGRA_EASY_MOBILE
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Services.FirebasePushNotificationService.Initialize();
        }

        /// <summary>
        /// Définit la fenêtre principale de l'application.
        /// Le Shell n'est volontairement pas créé ici : la connexion/droits doivent être connus avant sa construction.
        /// </summary>
        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new StartupConnectionView());
        }
    }
}
