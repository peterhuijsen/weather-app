import torch
import pandas as pd
from sklearn.preprocessing import StandardScaler
import numpy as np
import torch.nn as nn
import requests
from io import StringIO

url = "https://www.daggegevens.knmi.nl/klimatologie/daggegevens?stns=279&start=20221130&end=20231130"

resp = requests.get(url)
data = resp.text.replace(" ", "").split("\n")
data = data[46:]

data2 = ""
for line in data:
  data2 += line + "\n"
data = StringIO(data2[1:])

data = pd.read_csv(data)
data = data[['YYYYMMDD', "UG", "PX", "PN", "RH", "Q", "FG", "DR", "TG"]].copy(deep=True)[-365:]

data.dropna(inplace=True)
data["TG"] = data["TG"].divide(10)
data["YYYYMMDD"] = pd.to_datetime(data["YYYYMMDD"], format='%Y%m%d')
data['month'] = data['YYYYMMDD'].dt.month
data.set_index('YYYYMMDD')

train_data = data.copy(deep=True)
train_data = pd.get_dummies(data, columns = ['month'])

final_data = train_data.drop(['YYYYMMDD'], axis=1)

T = 15
D = final_data.shape[1]
N = len(data) - T

class LSTM(nn.Module):
  def __init__(self, input_size, hidden_size, num_layers, output_size):
    super(LSTM, self).__init__()
    self.hidden_size = hidden_size
    self.num_layers = num_layers

    self.rnn = nn.LSTM(
      input_size=input_size,
      hidden_size=hidden_size,
      num_layers=num_layers,
      batch_first=True)

    self.fc = nn.Linear(hidden_size, output_size)

  def forward(self, x):
    h0 = torch.zeros(self.num_layers, x.size(0), self.hidden_size).to(device)
    c0 = torch.zeros(self.num_layers, x.size(0), self.hidden_size).to(device)

    out, (hn, cn) = self.rnn(x, (h0.detach(), c0.detach()))

    out = self.fc(out[:, -1, :])
    return out
    
scaler = StandardScaler()
scaler.fit(final_data)
final_data = scaler.transform(final_data)

model = LSTM(D, 500, 2, 1)
device = torch.device("mps")
model.to(device)
model.load_state_dict(torch.load("model.pt", map_location=torch.device('mps')))
model.eval()

Xtest = np.zeros((N, T, D))
Xtest[-15:] = final_data[-15:]
Xtest = torch.from_numpy(Xtest.astype(np.float32))
Xtest = Xtest.to(device)

with torch.no_grad():
  pred = model(Xtest)

print(round(pred[-1:].item(), 1))