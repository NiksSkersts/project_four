using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LLU.Android.Controllers
{
    internal class EmailsViewHolder : RecyclerView.ViewHolder
    {
        public TextView Subject { get; set; }
        public TextView From { get; set; }
        public TextView Time { get; set; }
        public EmailsViewHolder(View itemView) : base(itemView)
        {
        }
    }
}