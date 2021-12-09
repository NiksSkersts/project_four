using System;
using System.Collections.Generic;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using MimeKit;

namespace LLU.Android.Models;

internal class EmailsViewAdapter : RecyclerView.Adapter {
    private readonly List<MimeMessage> _messages;

    public EmailsViewAdapter(List<MimeMessage> messages) => _messages = messages;

    public override int ItemCount => _messages.Count;
    public event EventHandler<int>? ItemClick;
    public event EventHandler<int>? ItemLongClick;

    private void OnClick(int position) {
        ItemClick?.Invoke(this, position);
    }

    private void OnLongClick(int position) {
        ItemLongClick?.Invoke(this, position);
    }

    public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position) {
        var vh = holder as EmailsViewHolder ?? throw new InvalidOperationException();
        vh.Subject.Text = _messages[position].Subject;
        vh.From.Text = $"{_messages[position].From}";
        vh.Time.Text =
            $"{_messages[position].Date.Day}/{_messages[position].Date.Month}/{_messages[position].Date.Year}";
    }

    public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType) {
        var item = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.EmailView, parent, false)!;
        EmailsViewHolder vh = new(item, OnClick, OnLongClick);
        return vh;
    }
}