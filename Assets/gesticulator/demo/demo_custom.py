import torch
from gesticulator.model.model import GesticulatorModel
from gesticulator.interface.gesture_predictor_custom import GesturePredictorCustom
from os.path import join
import numpy as np
import datetime
import json


def main(audio, text, save_dir):

    # 0. Check feature type based on the model
    feature_type, audio_dim = check_feature_type('Assets/gesticulator/demo/models/default.ckpt')

    # 1. Load the model
    model = GesticulatorModel.load_from_checkpoint('Assets/gesticulator/demo/models/default.ckpt', inference_mode=True)
    # This interface is a wrapper around the model for predicting new gestures conveniently
    gp = GesturePredictorCustom(model, feature_type)

    # 2. Predict the gestures with the loaded model
    motion = gp.predict_gestures(audio, text)

    current_time = datetime.datetime.now().strftime("%Y%m%d%H%M%S")
    x_rotations_csv_path = join(save_dir, f'{current_time}_rotations_x.csv')
    y_rotations_csv_path = join(save_dir, f'{current_time}_rotations_y.csv')
    z_rotations_csv_path = join(save_dir, f'{current_time}_rotations_Z.csv')

    np.savetxt(x_rotations_csv_path, motion[:, :, 0], delimiter=',')
    np.savetxt(y_rotations_csv_path, motion[:, :, 1], delimiter=',')
    np.savetxt(z_rotations_csv_path, motion[:, :, 2], delimiter=',')

    rtn = json.dumps(
        {
            'xRotationCsvPath': x_rotations_csv_path,
            'yRotationCsvPath': y_rotations_csv_path,
            'zRotationCsvPath': z_rotations_csv_path,
            'frameRate': 20,
            'frameCount': motion.shape[0]
        },
        separators=(',', ':')
    )

    return rtn


def check_feature_type(model_file):
    """
    Return the audio feature type and the corresponding dimensionality
    after inferring it from the given model file.
    """
    params = torch.load(model_file, map_location=torch.device('cpu'))

    # audio feature dim. + text feature dim.
    audio_plus_text_dim = params['state_dict']['encode_speech.0.weight'].shape[1]

    # This is a bit hacky, but we can rely on the fact that 
    # BERT has 768-dimensional vectors
    # We add 5 extra features on top of that in both cases.
    text_dim = 768 + 5

    audio_dim = audio_plus_text_dim - text_dim

    if audio_dim == 4:
        feature_type = "Pros"
    elif audio_dim == 64:
        feature_type = "Spectro"
    elif audio_dim == 68:
        feature_type = "Spectro+Pros"
    elif audio_dim == 26:
        feature_type = "MFCC"
    elif audio_dim == 30:
        feature_type = "MFCC+Pros"
    else:
        print("Error: Unknown audio feature type of dimension", audio_dim)
        exit(-1)

    return feature_type, audio_dim
