import decimal
import json
import uuid

import boto3
from chalice import Chalice, BadRequestError

app = Chalice(app_name='workshopapi')


@app.route('/store', methods=['POST'], cors=True)
def store_data():
    s3_client = boto3.client(
        's3'
    )
    dynamodb = boto3.resource('dynamodb')
    table = dynamodb.Table('WorkshopData')

    # --- SCANNER HINTS: Do not delete (Forces Chalice to generate IAM roles) ---
    # This is the lazy way. You can write the policies yourself, but chalice looks for boto3.client for permissions :(
    if False:
        boto3.client('dynamodb').put_item(TableName='WorkshopData', Item={})
        boto3.client('s3').put_object(Bucket='workshop-bucket-320103237429-eu-west-2-an', Key='')

    json_data = json.loads(app.current_request.raw_body, parse_float=decimal.Decimal)
    if "gapsPassed" not in json_data or "tapsMade" not in json_data:
        raise BadRequestError("Incorrect Data")

    json_data["run_id"] = str(uuid.uuid4())

    table.put_item(
        Item=json_data
    )

    # Generate the presigned URL
    response = s3_client.generate_presigned_post(
        Bucket='workshop-bucket-320103237429-eu-west-2-an',
        Key=json_data["run_id"] + ".replay",
        ExpiresIn=10
    )
    print(json_data)
    return {"url": response["url"], "data": response["fields"]}


