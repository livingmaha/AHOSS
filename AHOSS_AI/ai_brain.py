# /AHOSS_AI/ai_brain.py

from flask import Flask, request, jsonify
import logging

# Set up basic logging to see server activity
logging.basicConfig(level=logging.INFO)

# Initialize the Flask application
app = Flask(__name__)

@app.route('/predict', methods=['POST'])
def predict():
    """
    This endpoint receives scene data from Unity, processes it,
    and returns a decision.
    """
    # Get the JSON data from the POST request
    data = request.get_json()
    
    if not data or 'entities' not in data:
        logging.error("Received invalid data from Unity.")
        return jsonify({"error": "Invalid data format"}), 400

    logging.info(f"Received data for {len(data['entities'])} entities.")
    # For debugging, print the received data
    # import json
    # print(json.dumps(data, indent=2))

    # --- AI Logic (v0.1) ---
    # The goal is to find the first available "Enemy_Unit" and target it.
    target_id = None
    for entity in data['entities']:
        if entity.get('type') == 'Enemy_Unit':
            target_id = entity.get('id')
            logging.info(f"Found enemy! Targeting entity with ID: {target_id}")
            # Break the loop once we've found our first target
            break
    
    if not target_id:
        logging.info("No enemies found in the current scene.")

    # --- Prepare the response ---
    # The response is a simple JSON object with the chosen target's ID.
    # If no target was chosen, the ID will be null.
    response = {
        'target_id': target_id
    }
    
    return jsonify(response)

if __name__ == '__main__':
    # Run the Flask app on localhost, port 5000.
    # host='0.0.0.0' makes it accessible from the network.
    app.run(host='0.0.0.0', port=5000, debug=True)
