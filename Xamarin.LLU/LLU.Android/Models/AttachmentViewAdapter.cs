using Android.App;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;

namespace LLU.Android.Controllers
{
    public class AtatchmentDataAdapter : BaseAdapter<string>
    {

        List<string> items;

        Activity context;
        public AtatchmentDataAdapter(Activity context, List<string> items)
            : base()
        {
            this.context = context;
            this.items = items;
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override string this[int position]
        {
            get { return items[position]; }
        }
        public override int Count
        {
            get { return items.Count; }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = items[position];
            View view = convertView;
            if (view == null) // no view to re-use, create new
                view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.AttachmentListTemplate, parent, false);
            view.FindViewById<TextView>(Resource.Id.ALT_Name).Text = item;
            return view;
        }
    }
}