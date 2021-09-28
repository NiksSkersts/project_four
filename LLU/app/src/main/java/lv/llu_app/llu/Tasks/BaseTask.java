package lv.llu_app.llu.Tasks;

import android.os.Handler;
import android.os.Looper;

import javax.mail.MessagingException;
import java.io.IOException;
import java.util.concurrent.Executor;
import java.util.concurrent.Executors;

public interface BaseTask {
    Handler handler = new Handler(Looper.getMainLooper());
    Executor executor = Executors.newCachedThreadPool();
    Object call() throws IOException, MessagingException;
    void setDataAfterLoading(Object result);
}
