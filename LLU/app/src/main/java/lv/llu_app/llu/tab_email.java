package lv.llu_app.llu;

import android.os.Looper;
import androidx.appcompat.app.AppCompatActivity;
import android.os.Bundle;
import androidx.recyclerview.widget.DividerItemDecoration;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import lv.llu_app.llu.Email.Email;
import lv.llu_app.llu.Email.MessagesAdapter;

import java.util.concurrent.Executor;
import java.util.concurrent.Executors;
import java.util.concurrent.atomic.AtomicReference;

public class tab_email extends AppCompatActivity {
    private RecyclerView recyclerView;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_tab_email);
        recyclerView = findViewById(R.id.email_view);
        LinearLayoutManager mLinearLayoutManager = new LinearLayoutManager(tab_email.this,
                LinearLayoutManager.VERTICAL, false);
        recyclerView.setLayoutManager(mLinearLayoutManager);
        recyclerView.addItemDecoration(new DividerItemDecoration(tab_email.this,
                DividerItemDecoration.VERTICAL));
        recyclerView.setAdapter(new MessagesAdapter(this));
    }
}