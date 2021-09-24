package lv.llu_app.llu.Main;

import androidx.appcompat.app.AppCompatActivity;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;

import java.io.File;

import lv.llu_app.llu.Email.Email;
import lv.llu_app.llu.Email.EmailTab;
import lv.llu_app.llu.Login;
import lv.llu_app.llu.R;

public class MainActivity extends AppCompatActivity {
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        File file = new File("l");
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