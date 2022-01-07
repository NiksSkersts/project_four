using System;
using System.Collections.Generic;
using Android.Graphics;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using LLU.Models;

namespace LLU.Android.Models;

/// <summary>
///     Listview adapter that deals with creation of new emails object (EmailsDataHolder) when one gets added, removed etc.
/// </summary>
internal class EmailsViewAdapter : RecyclerView.Adapter {
    internal List<DatabaseData> _messages;

    public EmailsViewAdapter(List<DatabaseData> messages) => _messages = messages;

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
        if (_messages[position].PriorityFlag) {
            vh.Card.SetOutlineAmbientShadowColor(Color.PaleVioletRed);
            vh.Card.SetOutlineSpotShadowColor(Color.PaleVioletRed);
        }

        vh.Subject.Text = _messages[position].Subject;
        if (_messages[position].NewFlag) vh.Subject.SetTypeface(vh.Subject.Typeface, TypefaceStyle.Bold);
        vh.From.Text = $"{_messages[position].From}";
        var time = DateTimeOffset.FromUnixTimeSeconds(_messages[position].Time);
        vh.Time.Text =
            $"{time.Day}/{time.Month}/{time.Year}";
    }

    public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType) {
        var item = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.EmailView, parent, false)!;
        EmailsViewHolder vh = new(item, OnClick, OnLongClick);
        return vh;
    }
}