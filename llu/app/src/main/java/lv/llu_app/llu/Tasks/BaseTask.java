package lv.llu_app.llu.Tasks;

import lv.llu_app.llu.Async.CustomCallable;

public class BaseTask<R> implements CustomCallable<R> {
    @Override
    public void setDataAfterLoading(R result) {
    }
    @Override
    public R call() throws Exception {
        return null;
    }
}
