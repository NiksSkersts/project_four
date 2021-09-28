package lv.llu_app.llu.Email;

import android.content.Context;
import android.graphics.Typeface;
import android.view.HapticFeedbackConstants;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.LinearLayout;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.core.content.ContextCompat;
import androidx.recyclerview.widget.RecyclerView;

import java.io.IOException;
import java.util.Arrays;

import javax.mail.Flags;
import javax.mail.Folder;
import javax.mail.Message;
import javax.mail.MessagingException;

import lv.llu_app.llu.R;

public class MessagesAdapter extends RecyclerView.Adapter<MessagesAdapter.MyViewHolder> {
    private final Context mContext;

    public class MyViewHolder extends RecyclerView.ViewHolder implements View.OnLongClickListener {
        public TextView from, subject, message, timestamp;
        public LinearLayout messageContainer;

        public MyViewHolder(View view) {
            super(view);
            from = (TextView) view.findViewById(R.id.from);
            subject = (TextView) view.findViewById(R.id.txt_primary);
            message = (TextView) view.findViewById(R.id.txt_secondary);
            timestamp = (TextView) view.findViewById(R.id.timestamp);
            messageContainer = (LinearLayout) view.findViewById(R.id.message_container);
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
    public void onBindViewHolder(final MyViewHolder holder, final int position) {
        Message message = EmailTab.email_account.GetMessages(position);
        // displaying text view data
        try {
            holder.from.setText(message.getFrom().toString());
            holder.subject.setText(message.getSubject());
            holder.message.setText(EmailTab.email_account.Read(message));
            holder.timestamp.setText(message.getSentDate().toString());
        } catch (MessagingException | IOException e) {
            e.printStackTrace();
        }

        // change the font style depending on message read status
        try {
            applyReadStatus(holder, message);
        } catch (MessagingException e) {
            e.printStackTrace();
        }
    }

    @Override
    public long getItemId(int position) {
        return EmailTab.email_account.GetMessages(position).getMessageNumber();
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
        return EmailTab.email_account.GetMessages().length;
    }

    public void removeData(int position) throws MessagingException {
        EmailTab.email_account.GetMessages(position).setFlag(Flags.Flag.DELETED,true);
    }
}