using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishCenter.Domain.Constants;

public static class ClassSessionStatusConstants
{
    public const int Planned = 1;
    public const int Completed = 2;
    public const int Cancelled = 3;
    public const int Rescheduled = 4;
}
