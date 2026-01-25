using System;

public interface ICurrencyService
{
    int GetBalance(WalletAccount account, CurrencyType type);
    string GetFormatted(WalletAccount account, CurrencyType type, bool shortForm = true);
    string GetFormatted(int value, bool shortForm = true);

    bool TrySpend(WalletAccount account, CurrencyType type, int cost, string reason);
    void Grant(WalletAccount account, CurrencyType type, int amount, CurrencySource source, string reason);
    public int TransferAll(WalletAccount from, WalletAccount to);
    public int TransferCurrency(WalletAccount from, WalletAccount to, CurrencyType type);
    
    void EmitSnapshot(WalletAccount account, CurrencyType type, string reason = "snapshot");
    void ResetCurrencyToZero(WalletAccount account, CurrencyType type, bool notify = true, string reason = "reset_to_zero");
    void ResetAccountToZero(WalletAccount account, bool notify = true, string reason = "reset_to_zero");


    event Action<CurrencyChangedArgs> OnBalanceChanged;
}