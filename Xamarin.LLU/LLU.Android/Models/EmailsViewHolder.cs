using System;
using Android.Views;
using Android.Widget;
using AndroidX.CardView.Widget;
using AndroidX.RecyclerView.Widget;

namespace LLU.Android.Models;

internal class EmailsViewHolder : RecyclerView.ViewHolder {
    public EmailsViewHolder(View itemView, Action<int> listener, Action<int> longclicklistener) : base(itemView) {
        Subject = itemView.FindViewById<TextView>(Resource.Id.Subject)!;
        From = itemView.FindViewById<TextView>(Resource.Id.from)!;
        Time = itemView.FindViewById<TextView>(Resource.Id.time)!;
        Card = itemView.FindViewById<CardView>(Resource.Id.mainLinearLayout)!;
        itemView.Click += (sender, e) => listener(LayoutPosition);
        itemView.LongClick += (sender, e) => longclicklistener(LayoutPosition);
    }

    public TextView Subject { get; }
    public TextView From { get; }
    public TextView Time { get; }
    public CardView Card { get; }
}