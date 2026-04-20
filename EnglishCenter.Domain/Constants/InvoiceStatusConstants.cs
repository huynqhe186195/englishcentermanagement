using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishCenter.Domain.Constants;

public static class InvoiceStatusConstants
{
    public const int Unpaid = 1;
    public const int PartiallyPaid = 2; // giu lai de tuong thich DB, nhung business hien tai khong dung
    public const int Paid = 3;
    public const int Cancelled = 4;
}
