using System.Threading.Tasks;

public interface IModalService
{
    Task<ModalResult> ShowAsync(ModalRequest req);
    void Show(ModalRequest req, System.Action<ModalResult> onResult = null);
    void DismissActive(ModalResult reason = ModalResult.DismissedBackground);
    void DismissById(string correlationId, ModalResult reason = ModalResult.DismissedBackground);

}