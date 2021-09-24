package lv.llu_app.llu;
import android.os.Bundle;
import android.view.View;
import android.widget.EditText;

import androidx.appcompat.app.AppCompatActivity;

import lv.llu_app.llu.Tasks.LoginTask;

public class Login extends AppCompatActivity {
    public EditText usr_field;
    public EditText pass_field;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_login);
        usr_field = (EditText) findViewById(R.id.username_field);
        pass_field = (EditText) findViewById(R.id.password_field);
    }

    public void LoginClick(View view) {
        LoginTask loginTask = new LoginTask(usr_field.getText().toString(), pass_field.getText().toString());
        while (!loginTask.post){
            //Loading Screen and all
        }
        finish();
    }
}
