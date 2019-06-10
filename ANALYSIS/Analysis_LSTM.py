from keras.models import Model
from keras.layers import LSTM, Dense, Dropout, Activation, Input
from keras.optimizers import RMSprop
from sklearn.model_selection import StratifiedKFold
import numpy as np

dataset_num = 40
batch_size = 12
time_step = 672
data_dim = 32


# LSTM 모델을 만듦.
# activation1 - Activation function : 첫 번째 LSTM 레이어에 대한 activation 함수(ReLU)
# dropout1 - Dropout layer : 0.2의 probability를 갖고 있는 Dropout 레이어.
# activation2 - Activation function : 두 번째 LSTM 레이어에 대한 activation 함수(sigmoid)
def create_lstm_model():
    lstm_size = 32

    visible = Input(batch_shape=(batch_size, time_step, data_dim))
    lstm1_1, state_h, state_c = LSTM(lstm_size, return_state=True, return_sequences=True)(visible)
    states = [state_h, state_c]
    lstm1_2 = LSTM(lstm_size, return_sequences=True)(lstm1_1, states)
    activation1 = Activation('relu')(lstm1_2)
    dropout = Dropout(0.2)(activation1)
    lstm2 = LSTM(lstm_size)(dropout)
    activation2 = Activation('sigmoid')(lstm2)
    dense1 = Dense(2)(activation2)
    output = Activation('sigmoid')(dense1)

    model = Model(inputs=visible, outputs=output)
    rmsprop = RMSprop(lr=0.001)
    model.compile(optimizer=rmsprop, loss='mse', metrics=['accuracy'])

    print(model.summary())

    return model


def train_evaluate_lstm_model(trained_model, w_dat, w_label, tr_indice, te_indice):
    tr_x = np.reshape(w_dat[tr_indice], (-1, time_step, data_dim))
    tr_y = np.reshape(w_label[tr_indice], (-1, 2))
    te_x = np.reshape(w_dat[te_indice], (-1, time_step, data_dim))
    te_y = np.reshape(w_label[te_indice], (-1, 2))

    total_train_acc = []
    total_train_loss = []
    total_val_acc = []
    total_val_loss = []

    # 영상 데이터를 학습한다.
    hist = trained_model.fit(tr_x, tr_y, epochs=30, batch_size=batch_size, verbose=1, validation_data=(te_x, te_y))
    print("loss:", np.mean(hist.history['loss']), "- acc:", np.mean(hist.history['acc']))
    print("val_loss:", np.mean(hist.history['val_loss']), "- val_acc:", np.mean(hist.history['val_acc']))
    total_train_loss.append(hist.history['loss'])
    total_train_acc.append(hist.history['acc'])

    return trained_model


data = np.load('lstm_data.npz')
total_x = np.array(data['data'])
total_y = np.array(data['label'])
data.close()

# 트레이닝 데이터 셋 = 1번째 참가자, 테스트 데이터 셋 = 2번째 참가자
X_train = np.array(total_x[0]).reshape((40, 32, 12, -1))
train_y = np.array(total_y[0][0])
X_test = np.array(total_x[1]).reshape((40, 32, 12, -1))
test_y = np.array(total_y[1][0])

# input 데이터()를 전치.
X_train = np.transpose(X_train, [0, 2, 3, 1])
X_test = np.transpose(X_test, [0, 2, 3, 1])

Y_train = np.empty((dataset_num, batch_size, 2))
Y_test = np.empty((dataset_num, batch_size, 2))
for idx in range(dataset_num):
    for idx2 in range(batch_size):
        if train_y[idx] == 1:
            Y_train[idx][idx2] = [0., 1.]
        else:
            Y_train[idx][idx2] = [1., 0.]
        if test_y[idx] == 1:
            Y_test[idx][idx2] = [0., 1.]
        else:
            Y_test[idx][idx2] = [1., 0.]

data, labels = X_train.reshape((dataset_num, -1, time_step, data_dim)), Y_train.reshape((dataset_num, -1, 2))
predict_data, predict_label = X_test.reshape((dataset_num, -1, time_step, data_dim)), Y_test.reshape(
    (dataset_num, -1, 2))

#  40개의 임의의 데이터를 4개의 케이스로 분리.
n_folds = 4
skf = StratifiedKFold(n_folds, shuffle=True)
X = np.ones((40, 1), dtype=np.float32)
y = np.ones((40, 1), dtype=np.float32)

model = create_lstm_model()

# 4회동안 랜덤한 트레이닝 인덱스 셋과 테스트 인덱스 셋을 얻어 학습에 이용.
for k, (train_indices, test_indices) in enumerate(skf.split(X, y)):
    print("Running Fold", k + 1, "/", n_folds)
    model = train_evaluate_lstm_model(model, data, labels, train_indices, test_indices)

model.save('lstm_model.h5')

#결과값 예측.
for predict_idx in range(40):
    result = model.predict(predict_data[predict_idx], verbose=1, batch_size=batch_size)
    print("====Video #", predict_idx, "Prediction====")
    result_left = []
    result_right = []
    for i in range(batch_size):
        result_left.append(abs(result[i][0] - predict_label[predict_idx][i][0]))
        result_right.append(abs(result[i][1] - predict_label[predict_idx][i][1]))

    print("Predict : [", np.mean(result_left), ", ", np.mean(result_right), "], Real : ", predict_label[predict_idx][0])
