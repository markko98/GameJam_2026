public class ServiceProvider
{
    public static AudioService audioService;
    public static IPersistentStorageManager storage = new LocalPersistentStorageManager();
    public static CurrencyService currencyService;
    public static ModalService modalService;
    public static IModalService tutorialModalService;
}