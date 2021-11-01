using AndroidX.RecyclerView.Widget;
using Android.Views;
using System;
using System.Collections.Generic;
using MimeKit;
using LLU.Android.LLU.Models;

namespace LLU.Android.Controllers
{
    internal class EmailsViewAdapter : RecyclerView.Adapter
    {
        public event EventHandler<int> ItemClick; 
        public List<MimeMessage> _messages;
        void OnClick (int position)
        {
            ItemClick?.Invoke(this, position);
        }
        public EmailsViewAdapter(List<MimeMessage> messages) => _messages = messages;
        public override int ItemCount => _messages.Count;
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            EmailsViewHolder vh = holder as EmailsViewHolder ?? throw new InvalidOperationException();
            vh.Subject.Text = _messages[position].Subject;
            vh.From.Text = _messages[position].From.ToString();
            vh.Time.Text = _messages[position].Date.ToString();
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View item = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.EmailView, parent, false);
            EmailsViewHolder vh = new(item,OnClick);
            return vh;
        }
    }
}