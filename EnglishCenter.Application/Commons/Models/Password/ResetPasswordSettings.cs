using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishCenter.Application.Common.Models;

public class ResetPasswordSettings
{
    public int TokenExpirationMinutes { get; set; }
    public string ResetUrl { get; set; } = string.Empty;
}
