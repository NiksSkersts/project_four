package lv.llu_app.llu.Email;

import android.content.Context;
import android.graphics.Typeface;
import android.os.Looper;
import android.view.HapticFeedbackConstants;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.LinearLayout;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.core.content.ContextCompat;
import androidx.recyclerview.widget.RecyclerView;

import javax.mail.*;

import lv.llu_app.llu.Model.EmailUser;
import lv.llu_app.llu.Model.User;
import lv.llu_app.llu.R;
import org.jetbrains.annotations.NotNull;

public class MessagesAdapter extends RecyclerView.Adapter<MessagesAdapter.MyViewHolder> {
    private final Context mContext;
    private final EmailUser emailUser = User.email_account;

    public static class MyViewHolder extends RecyclerView.ViewHolder implements View.OnLongClickListener {
        public TextView from, subject, message, timestamp;
        public LinearLayout messageContainer;

        public MyViewHolder(View view) {
            super(view);
            from = view.findViewById(R.id.from);
            subject = view.findViewById(R.id.txt_primary);
            message = view.findViewById(R.id.txt_secondary);
            timestamp = view.findViewById(R.id.timestamp);
            messageContainer = view.findViewById(R.id.message_container);
            view.setOnLongClickListener(this);
        }

        @Override
        public boolean onLongClick(View view) {
            view.performHapticFeedback(HapticFeedbackConstants.LONG_PRESS);
            return true;
        }
    }

    public MessagesAdapter(Context mContext) {
        this.mContext = mContext;
    }

    @NonNull
    @Override
    public MyViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
        View itemView = LayoutInflater.from(parent.getContext())
                .inflate(R.layout.message_list_row, parent, false);
        return new MyViewHolder(itemView);
    }

    @Override
    public void onBindViewHolder(@NotNull final MyViewHolder holder, final int position) {
        Message message = emailUser.messages.get(position);
        try {
            holder.from.setText(message.getFrom()[0].toString());
            holder.subject.setText(message.getSubject());
            holder.message.setText(Email.Read(message));
            holder.timestamp.setText(message.getSentDate().toString());
            applyReadStatus(holder, message);
        } catch (MessagingException e) {
            e.printStackTrace();
        }
    }

    @Override
    public long getItemId(int position) {
        return emailUser.messages.get(position).getMessageNumber();
    }

    private void applyReadStatus(MyViewHolder holder, Message message) throws MessagingException {
        if (message.getFlags().contains(Flags.Flag.SEEN)) {
            holder.from.setTypeface(null, Typeface.NORMAL);
            holder.subject.setTypeface(null, Typeface.NORMAL);
            holder.from.setTextColor(ContextCompat.getColor(mContext, R.color.subject));
            holder.subject.setTextColor(ContextCompat.getColor(mContext, R.color.message));
        } else {
            holder.from.setTypeface(null, Typeface.BOLD);
            holder.subject.setTypeface(null, Typeface.BOLD);
            holder.from.setTextColor(ContextCompat.getColor(mContext, R.color.from));
            holder.subject.setTextColor(ContextCompat.getColor(mContext, R.color.subject));
        }
    }

    @Override
    public int getItemCount() {
        return emailUser.messages.size();
    }

    public void removeData(int position) throws MessagingException {
        Message message = emailUser.messages.get(position);
        message.setFlag(Flags.Flag.DELETED,true);
    }
}