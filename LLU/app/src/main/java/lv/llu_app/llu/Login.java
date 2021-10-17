package lv.llu_app.llu;

import android.view.View;
import android.widget.EditText;
import android.widget.Toast;
import androidx.appcompat.app.AppCompatActivity;
import android.os.Bundle;
import lv.llu_app.llu.Tasks.LoginTask;

import java.io.*;

public class Login extends AppCompatActivity {
    public EditText usr_field;
    public EditText pass_field;
    boolean login = true;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_login);
        usr_field = findViewById(R.id.username_field);
        pass_field = findViewById(R.id.password_field);
    }

    public void LoginClick(View view) {
        //String username = usr_field.getText().toString();
        //String password = pass_field.getText().toString();
        String username = "775a6cabd3ec54";
        String password = "07c19676c13067";
        LoginTask loginTask = new LoginTask(username,password);
            while(!loginTask.post){
                //todo loading screen
            }
            if (!loginTask.err) {
                login = false;
                HandleIO(username,password);
                finish();
            }
    }

    private void HandleIO(String username,String password) {
        File dir = this.getFilesDir();
        File file = new File(dir,"login");
        try {
            if(!file.createNewFile()) return;
            FileOutputStream fileOutputStream = openFileOutput("login",MODE_PRIVATE);
            OutputStreamWriter osw = new OutputStreamWriter(fileOutputStream);
            osw.write(username);
            osw.write("\n");
            osw.write(password);
            osw.flush();
            osw.close();

        } catch (IOException e) {
            e.printStackTrace();
        }
    }
}