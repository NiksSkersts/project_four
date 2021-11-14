using Android.Views;
using AndroidX.RecyclerView.Widget;
using LLU.Android.LLU.Models;
using MimeKit;
using System;
using System.Collections.Generic;

namespace LLU.Android.Controllers
{
    internal class EmailsViewAdapter : RecyclerView.Adapter
    {
        public event EventHandler<int> ItemClick;
        public event EventHandler<int> ItemLongClick;
        public List<MimeMessage> _messages;
        void OnClick(int position)
        {
            ItemClick?.Invoke(this, position);
        }
        void OnLongClick(int position)
        {
            ItemLongClick?.Invoke(this, position);
        }
        public EmailsViewAdapter(List<MimeMessage> messages) => _messages = messages;
        public override int ItemCount => _messages.Count;
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            EmailsViewHolder vh = holder as EmailsViewHolder ?? throw new InvalidOperationException();
            vh.Subject.Text = _messages[position].Subject;
            vh.From.Text = $"{_messages[position].From}";
            vh.Time.Text = $"{_messages[position].Date.Day}/{_messages[position].Date.Month}/{_messages[position].Date.Year}";
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View item = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.EmailView, parent, false);
            EmailsViewHolder vh = new(item, OnClick,OnLongClick);
            return vh;
        }
    }
}