package lv.llu_app.llu.Async;

import android.os.Handler;
import android.os.Looper;

import java.util.concurrent.Executor;
import java.util.concurrent.Executors;

public class TaskRunner {
    private final Handler handler = new Handler(Looper.getMainLooper());
    private final Executor executor = Executors.newCachedThreadPool();

    public <R> void ExecuteAsync(CustomCallable<R> callable){
        executor.execute(new RunnableTask<R>(handler, callable));
    }
    public static class RunnableTask<R> implements Runnable{
        private final Handler handler;
        private final CustomCallable<R> callable;

        public RunnableTask(Handler handler, CustomCallable<R> callable) {
            this.handler = handler;
            this.callable = callable;
        }

        @Override
        public void run() {
            try {
                final R result = callable.call();
                handler.post(new RunnableTaskForHandler(callable, result));
            } catch (Exception e) {
            }
        }
    }

    public static class RunnableTaskForHandler<R> implements Runnable{

        private CustomCallable<R> callable;
        private R result;

        public RunnableTaskForHandler(CustomCallable<R> callable, R result) {
            this.callable = callable;
            this.result = result;
        }

        @Override
        public void run() {
            callable.setDataAfterLoading(result);
        }
    }
}
