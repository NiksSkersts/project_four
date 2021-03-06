using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;

namespace LLU.Android.Models;

/// <summary>
///     Listview adapter that creates and updates the list to add and output all the attachments that have been bundled
///     with the MimeMessage.
/// </summary>
public class AttachmentDataAdapter : BaseAdapter<string> {
    private readonly Activity _context;
    private readonly List<string> _items;

    public AttachmentDataAdapter(Activity context, List<string> items) {
        _context = context;
        _items = items;
    }

    public override string this[int position] => _items[position];

    public override int Count => _items.Count;

    public override long GetItemId(int position) => position;

    public override View GetView(int position, View? convertView, ViewGroup? parent) {
        var view = convertView;
        if (convertView is null) view = _context.LayoutInflater.Inflate(Resource.Layout.AttachmentListTemplate, null);
        var item = _items[position];
        view.FindViewById<TextView>(Resource.Id.ALT_Name)!.Text = item;
        return view;
    }
}