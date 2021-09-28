package lv.llu_app.llu;

import android.content.Intent;
import android.view.View;
import androidx.appcompat.app.AppCompatActivity;
import android.os.Bundle;
import lv.llu_app.llu.Email.EmailTab;

import java.io.File;

public class Main extends AppCompatActivity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        File file = new File("login.bin");
        if (!file.exists())
        {
            Intent i = new Intent(this, Login.class);
            startActivity(i);
        }else{
            //Load from file
        }
    }

    public void emailbtn_click(View view) {
        Intent i = new Intent(this, EmailTab.class);
        startActivity(i);
    }
}