using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using System;

namespace LLU.Android.LLU.Models
{
    internal class EmailsViewHolder : RecyclerView.ViewHolder
    {
        public TextView Subject { get; set; }
        public TextView From { get; set; }
        public TextView Time { get; set; }
        public EmailsViewHolder(View itemView, Action<int> listener,Action<int> longclicklistener) : base(itemView)
        {
            Subject = itemView.FindViewById<TextView>(Resource.Id.Subject);
            From = itemView.FindViewById<TextView>(Resource.Id.@from);
            Time = itemView.FindViewById<TextView>(Resource.Id.time);
            itemView.Click += (sender, e) => listener(base.LayoutPosition);
            itemView.LongClick += (sender,e) => longclicklistener(base.LayoutPosition);
        }
    }
}