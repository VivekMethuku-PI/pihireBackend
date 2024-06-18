using System;
using System.Collections.Generic;

namespace PiHire.DAL.Entities;

public partial class PhCurrencyExchangeRate
{
    public int Id { get; set; }

    public string FromCurrency { get; set; }

    public string ToCurrency { get; set; }

    public decimal ExchangeRate { get; set; }

    public DateTime CreatedDate { get; set; }
}
