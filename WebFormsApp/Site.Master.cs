using System;
using System.Web;
using System.Web.UI;

namespace WebFormsApp
{
    public partial class SiteMaster : MasterPage
    {
        public string BlazorBase { get; set; }
        protected void Page_Load(object sender, EventArgs e)
        {
            BlazorBase = Request.GetOwinContext().Request.Uri.GetLeftPart(UriPartial.Authority) + "/";
        }
    }
}