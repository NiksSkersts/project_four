package lv.llu_app.llu;

import android.view.View;
import android.widget.EditText;
import android.widget.Toast;
import androidx.appcompat.app.AppCompatActivity;
import android.os.Bundle;
import lv.llu_app.llu.Tasks.LoginTask;

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
            LoginTask loginTask = new LoginTask(usr_field.getText().toString(), pass_field.getText().toString());
            while (!loginTask.post && !loginTask.err){
                //Loading Screen and all
            }
            if (!loginTask.err) {
                login = false;
                finish();
            }else{
                Toast.makeText(this,"Error logging in, check your creds",Toast.LENGTH_LONG).show();
            }
    }
}