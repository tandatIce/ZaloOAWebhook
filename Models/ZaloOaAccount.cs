using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZaloOAWebhook.Models;

public partial class ZaloOaAccount
{
    public int Id { get; set; }

    public string App_Id { get; set; } = null!;

    public string OA_Id { get; set; } = null!;

    public string? User_Id_By_App { get; set; }

    public string? BrandName { get; set; }

    public string DatabaseName { get; set; } = null!;

    public string RefreshToken { get; set; } = null!;

    public string AccessToken { get; set; } = null!;

    public string Secret_key { get; set; } = null!;
    public DateTime UpdateDate { get; set; }
}
