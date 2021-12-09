using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;

namespace LLU.Android.Models;

public class AttachmentDataAdapter : BaseAdapter<string> {
    private readonly List<string> _items;

    private Activity _context;

    public AttachmentDataAdapter(Activity context, List<string> items) {
        _context = context;
        _items = items;
    }

    public override string this[int position] => _items[position];

    public override int Count => _items.Count;

    public override long GetItemId(int position) => position;

    public override View GetView(int position, View? convertView, ViewGroup? parent) {
        var item = _items[position];
        if (convertView == null) return new View(Application.Context);
        convertView.FindViewById<TextView>(Resource.Id.ALT_Name)!.Text = item;
        return convertView;
    }
}