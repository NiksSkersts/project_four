using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using AndroidX.RecyclerView.Widget;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MimeKit;

namespace LLU.Android.Controllers
{
    internal class EmailsViewAdapter : RecyclerView.Adapter
    {
        public List<MimeMessage> _messages;
        public EmailsViewAdapter(List<MimeMessage> messages)
        {
            _messages = messages;
        }

        public override int ItemCount
        {
            get { return _messages.Count; }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            EmailsViewHolder vh = holder as EmailsViewHolder;
            vh.Subject.Text = _messages[position].Subject;
            vh.From.Text = _messages[position].From.ToString();
            vh.Time.Text = _messages[position].Date.ToString();
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View item = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.EmailView, parent, false);
            EmailsViewHolder vh = new EmailsViewHolder(item);
            return vh;
        }
    }
}