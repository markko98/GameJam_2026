using System.Threading.Tasks;

public interface IModalPresenter
{
    Task<ModalResult> Present(ModalRequest req);
    void DismissActive(ModalResult reason = ModalResult.DismissedBackground);
    void DismissById(string correlationId, ModalResult reason = ModalResult.DismissedBackground);

}