using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishCenter.Application.Features.Auth.Dtos;

public class ForgotPasswordRequestDto
{
    public string EmailOrUserName { get; set; } = string.Empty;
}