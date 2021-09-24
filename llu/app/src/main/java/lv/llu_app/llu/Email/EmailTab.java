package lv.llu_app.llu.Email;

import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.DividerItemDecoration;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import android.os.Bundle;

import java.io.IOException;

import javax.mail.Folder;
import javax.mail.Message;
import javax.mail.MessagingException;
import javax.mail.Store;

import lv.llu_app.llu.Main.MainActivity;
import lv.llu_app.llu.R;

public class EmailTab extends AppCompatActivity {
    public static Email email_account;
    private RecyclerView recyclerView;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_email_tab);
        recyclerView = findViewById(R.id.recyclerView);
        LinearLayoutManager mLinearLayoutManager = new LinearLayoutManager(EmailTab.this,
                LinearLayoutManager.VERTICAL, false);
        recyclerView.setLayoutManager(mLinearLayoutManager);
        recyclerView.addItemDecoration(new DividerItemDecoration(EmailTab.this,
                DividerItemDecoration.VERTICAL));
        recyclerView.setAdapter(new MessagesAdapter(this));
    }
}