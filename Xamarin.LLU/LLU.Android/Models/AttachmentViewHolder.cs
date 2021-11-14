using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;

namespace LLU.Android.LLU.Models
{
    internal class AttachmentViewHolder : RecyclerView.ViewHolder
    {
        public TextView Name { get; set; }
        public AttachmentViewHolder(View itemView) : base(itemView)
        {
            Name = new TextView(itemView.Context);
        }
    }
}