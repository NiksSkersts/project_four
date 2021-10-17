package lv.llu_app.llu;

import android.content.Intent;
import android.view.View;
import androidx.appcompat.app.AppCompatActivity;
import android.os.Bundle;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileReader;
import java.io.IOException;
import java.util.Arrays;
import java.util.Objects;

public class Main extends AppCompatActivity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        File LoginFile = null;
        File dir  = this.getFilesDir();
        File[] files = dir.listFiles();
        boolean exists = false;
        if (Arrays.stream(files).findAny().isPresent()){
            LoginFile = new File(dir, "login");
            exists=true;
        }
        if (!exists) {

            Intent i = new Intent(this, Login.class);
            startActivity(i);
        } else {
            //Read text from file
            String[] text = new String[2];
            try {
                BufferedReader br = new BufferedReader(new FileReader(LoginFile));
                String line;
                int i = 0;
                while ((line = br.readLine()) != null) {
                    text[i] = line;
                    if (text[i] == null || Objects.equals(text[i], "")) {
                        throw new IOException();

                    }
                    i++;
                }
                br.close();
            } catch (IOException e) {
                //todo proper err handling
                LoginFile.delete();
                exists = false;
                Intent i = new Intent(this, Login.class);
                startActivity(i);
            }

        }
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
    }

    public void emailbtn_click(View view) {
        Intent i = new Intent(this, tab_email.class);
        startActivity(i);
    }
}