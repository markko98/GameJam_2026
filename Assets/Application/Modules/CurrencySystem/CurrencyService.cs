using System;
using System.Collections.Generic;
using System.Globalization;

public sealed class CurrencyService : ICurrencyService
{
    // config
    private readonly Dictionary<CurrencyType, int> startingBalance = new Dictionary<CurrencyType, int>()
    {
        {CurrencyType.Coins, 200}, 
        {CurrencyType.Gems, 10}, 
        {CurrencyType.MechanicParts, 1}, 
    };
    private bool useShortForm = true;
    private IFormatProvider longFormCulture = CultureInfo.InvariantCulture;

    private readonly Wallet wallet = new();

    public event Action<CurrencyChangedArgs> OnBalanceChanged;

    public CurrencyService()
    {
        Initialize();
    }

    private void Initialize()
    {
        var balances = ServiceProvider.storage.LoadWallet(WalletAccount.Main, startingBalance);
        wallet.SetInitialBalances(WalletAccount.Main, balances);
        wallet.OnBalanceChanged += HandleBalanceChanged;
    }
    public void EmitSnapshot(WalletAccount account, CurrencyType type, string reason = "snapshot")
    {
        int value = wallet.GetBalance(account, type);

        var args = new CurrencyChangedArgs(
            account: account,
            type: type,
            before: value,
            after: value,
            delta: 0,
            source: CurrencySource.Debug,
            reason: reason,
            formatter: v => Format(v, useShortForm)
        );

        OnBalanceChanged?.Invoke(args);
    }
    public void ResetCurrencyToZero(WalletAccount account, CurrencyType type, bool notify = true, string reason = "reset_to_zero")
    {
        int before = wallet.GetBalance(account, type);
        wallet.SetInitialBalance(account, type, 0);
        if (!notify) return;

        var args = new CurrencyChangedArgs(
            account: account,
            type: type,
            before: before,
            after: 0,
            delta: -before,
            source: CurrencySource.Debug,
            reason: reason,
            formatter: v => Format(v, useShortForm)
        );
        OnBalanceChanged?.Invoke(args);
    }
    public void ResetAccountToZero(WalletAccount account, bool notify = true, string reason = "reset_to_zero")
    {
        var allBalances = wallet.GetAllBalances(account);
        foreach (var balance in allBalances)
        {
            ResetCurrencyToZero(account, balance.Key, notify, reason);
        }
    }

    public int GetBalance(WalletAccount account, CurrencyType type)
        => wallet.GetBalance(account, type);
    
    public Dictionary<CurrencyType, int> GetAllBalance(WalletAccount account)
        => wallet.GetAllBalances(account);

    public string GetFormatted(WalletAccount account, CurrencyType type, bool shortForm = true)
        => Format(wallet.GetBalance(account, type), shortForm);

    public string GetFormatted(int value, bool shortForm = true)
        => Format(value, shortForm);

    public bool TrySpend(WalletAccount account, CurrencyType type, int cost, string reason)
    {
        if (cost <= 0) return true;
        if (!wallet.CanAfford(account, type, cost)) return false;

        wallet.ApplyBalanceChange(new CurrencyChange(
            type: type,
            amount: -cost,
            source: CurrencySource.Spend,
            reason: reason,
            account: account
        ));
        return true;
    }
    public bool CanAfford(WalletAccount account, CurrencyType type, int cost)
    {
        return cost <= 0 || wallet.CanAfford(account, type, cost);
    }

    public void Grant(WalletAccount account, CurrencyType type, int amount, CurrencySource source, string reason)
    {
        if (amount <= 0) return;

        wallet.ApplyBalanceChange(new CurrencyChange(
            type: type,
            amount: amount,
            source: source,
            reason: reason,
            account: account
        ));
    }
    public int TransferAll(WalletAccount from, WalletAccount to) => wallet.TransferAll(from, to);
    public int TransferCurrency(WalletAccount from, WalletAccount to, CurrencyType type) => wallet.TransferCurrency(from, to, type);

    // event bridge
    private void HandleBalanceChanged(
        WalletAccount account,
        CurrencyType currency,
        int before,
        int after,
        CurrencyChange change)
    {
        // Persist only MAIN account changes
        if (account == WalletAccount.Main)
        {
            ServiceProvider.storage.SaveCurrency(currency, after);
        }

        var args = new CurrencyChangedArgs(
            account: account,
            type: currency,
            before: before,
            after: after,
            delta: change.amount,
            source: change.source,
            reason: change.reason,
            formatter: v => Format(v, useShortForm)
        );
        
        ServiceProvider.storage.SaveWallet(WalletAccount.Main, GetAllBalance(WalletAccount.Main));

        OnBalanceChanged?.Invoke(args);
    }

    // Formatting
    private string Format(int value, bool shortForm)
        => shortForm
            ? NumberFormatter.ShortFormat(value)
            : NumberFormatter.LongFormat(value, longFormCulture);

    public void SetShortForm(bool enabled) => useShortForm = enabled;
    public void SetLongFormCulture(IFormatProvider culture) => longFormCulture = culture ?? CultureInfo.InvariantCulture;
}

public readonly struct CurrencyChangedArgs
{
    public readonly WalletAccount account;
    public readonly CurrencyType type;
    public readonly int before;
    public readonly int after;
    public readonly int delta;
    public readonly CurrencySource source;
    public readonly string reason;

    public readonly string beforeFormatted;
    public readonly string afterFormatted;

    public CurrencyChangedArgs(
        WalletAccount account,
        CurrencyType type,
        int before,
        int after,
        int delta,
        CurrencySource source,
        string reason,
        Func<int, string> formatter)
    {
        this.account = account;
        this.type = type;
        this.before = before;
        this.after = after;
        this.delta = delta;
        this.source = source;
        this.reason = reason;

        beforeFormatted = formatter(before);
        afterFormatted  = formatter(after);
    }
}

