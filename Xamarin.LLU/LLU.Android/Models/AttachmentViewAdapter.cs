using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using LLU.Android.LLU.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Essentials;

namespace LLU.Android.Controllers
{
    internal class AttachmentViewAdapter : RecyclerView.Adapter
    {
        public Dictionary<string, string> _filenames;
        public AttachmentViewAdapter(Dictionary<string, string> filenames) => _filenames = filenames;
        public override int ItemCount => _filenames.Count;
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            AttachmentViewHolder vh = holder as AttachmentViewHolder ?? throw new InvalidOperationException();
            vh.Name.Text = _filenames.Keys.ToArray()[position];
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View item = new(parent.Context);
            item.Click += Item_Click;
            AttachmentViewHolder vh = new(item);
            return vh;
        }

        private void Item_Click(object sender, EventArgs e)
        {
            var attachment = sender as TextView;
            var filename = attachment.Text;
            Launcher.OpenAsync(new OpenFileRequest()
            {
                File = new ReadOnlyFile(_filenames.Single(key => key.Key.Equals(filename)).Value)
            });
        }
    }
}