import logging
from datetime import datetime
from http import HTTPStatus
from os import environ, listdir, stat

import boto3
import jsonpickle
import requests

import conversion_constants as cc
from models import DetailRecordModel, HeaderModel, RequestModel, TrailerModel

logger = logging.getLogger()
logger.setLevel(logging.DEBUG)
run_on_aws = True if environ.get("IMPORT_RATES_URL") else False
import_rates_url = (
    environ.get("IMPORT_RATES_URL") if run_on_aws else cc.IMPORT_RATES_URL
)
currency_rates_file_name = ""
s3_session = boto3.Session(
    aws_access_key_id=environ.get("ACCESS_KEY_ID"),
    aws_secret_access_key=environ.get("SECRET_ACCESS_KEY"),
)


def lambda_handler(event, context):
    """
    Funtction that processes events.
    Arguments:
        event: The event dict that contains the parameters sent when the function is invoked.
        context: The context in which the function is called.
    Return: The result of the action.
    """
    import_rates()


def import_rates():
    """
    Function that imports exchange rates to the API.
    """

    logger.info("Importing exchange rates...")
    data = get_exchange_rates_for_import()
    if data.error:
        logger.error("Aborting exchange rates import because of the error")
        return
    json_data = jsonpickle.encode(get_exchange_rates_for_import(), unpicklable=False)
    res = requests.post(
        import_rates_url,
        data=json_data,
        verify=False,
        headers={"Content-Type": "application/json"},
    )
    if res.status_code == HTTPStatus.OK:
        logger.info("Exchange rates imported successfully")
    else:
        logger.error("Error importing exchange rates")
        logger.error(f"Status code: {res.status_code}")
        logger.error(res.text)


def get_exchange_rates_for_import():
    """
    Function that reads and processes the file.
    Return: Exchange rates for import
    """

    if not file_exists_and_not_empty():
        return log_error_and_return("File does not exist or it is empty")

    if not run_on_aws:
        with open(
            cc.CURRENCY_RATES_FILE_PATH + currency_rates_file_name, mode="rt"
        ) as file:
            return process_file(file.readlines())
    else:
        file = (
            s3_session.client("s3")
            .get_object(
                Bucket=environ.get("S3_BUCKET_NAME"), Key=currency_rates_file_name
            )["Body"]
            .read()
            .decode("utf-8")
            .splitlines()
        )
        return process_file(file)


def log_error_and_return(error_message: str) -> RequestModel:
    """
    Function that logs error and creates RequestModel containing error message.
    Arguments:
        error_message: Error message.
    Return: RequestModel containing error message.
    """

    ret = RequestModel(header=None, detail_records=[], trailer=None, error=None)
    logger.error(error_message)
    ret.error = error_message
    return ret


def process_file(file_lines) -> RequestModel:
    """
    Function processes file lines into RequestModel.
    Arguments:
        file_lines: file lines
    Return: RequestModel containing data from file lines.
    """

    ret = RequestModel(header=None, detail_records=[], trailer=None, error=None)
    file_lines = [line for line in (line.strip() for line in file_lines) if line]

    if not is_file_format_valid(file_lines):
        return log_error_and_return("Improper file format")

    ret.header = convert_file_line_to_header(file_lines[0])
    ret.trailer = convert_file_line_to_trailer(file_lines[-1])
    if ret.trailer.total_records != len(file_lines) - 2:
        return log_error_and_return(
            "Number of data records does not match with trailer"
        )

    # skip header and trailer
    for line in file_lines[1:-1]:
        detail_record = convert_file_line_to_detail_record(line)
        if is_data_record_allowed(detail_record):
            ret.detail_records.append(detail_record)

    if not is_hash_valid(ret):
        return log_error_and_return("Hash do not match")

    return ret


def file_exists_and_not_empty() -> bool:
    """
    Function that checks whether the file exists and whether it is empty.
    Return: True if the file exists and it is not empty. False otherwise.
    """
    if run_on_aws:
        return file_exists_and_not_empty_aws()

    return file_exists_and_not_empty_local_file_storage()


def file_exists_and_not_empty_aws() -> bool:
    """
    Function that checks whether the file exists on AWS S3 bucket, and whether it is empty.
    Return: True if the file exists and it is not empty. False otherwise.
    """

    rates_bucket = s3_session.resource("s3").Bucket(environ.get("S3_BUCKET_NAME"))
    file_names = [
        file.key
        for file in rates_bucket.objects.all()
        if is_file_name_valid(file.key) and int(file.get()["ContentLength"])
    ]

    if not file_names:
        return False

    set_currency_rates_file_name(file_names)

    return True


def file_exists_and_not_empty_local_file_storage() -> bool:
    """
    Function that checks whether the file exists on local file storage, and whether it is empty.
    Return: True if the file exists and it is not empty. False otherwise.
    """

    file_names = [
        file_name
        for file_name in listdir(cc.CURRENCY_RATES_FILE_PATH)
        if is_file_name_valid(file_name)
    ]

    if not file_names:
        return False

    set_currency_rates_file_name(file_names)

    if stat(currency_rates_file_name).st_size == 0:
        return False
    return True


def is_file_name_valid(file_name) -> bool:
    """
    Function that check wheather the exchange rates file name is valid.
    File name is valid if its exstension matches with the one defined in the conversion contstants,
    if it contains two underscore characters, and string between two underscores is number.
    Return: True, if the file name is valid. Otherwise false.
    """

    return (
        file_name.endswith(cc.CURRENCY_RATES_FILE_EXTENSION)
        and file_name.count("_") == 2
        and file_name.split("_")[1].isnumeric()
    )


def set_currency_rates_file_name(file_names):
    """
    Function that sets the latest currency file name from the list of possible file names.
    Latest file name is determined from its name section between two underscores, which
    represents date in the format of YYMMDD.
    Arguments:
        file_lines: File lines.
    """

    global currency_rates_file_name
    for file_name in file_names:
        if not currency_rates_file_name:
            currency_rates_file_name = file_name
        elif int(file_name.split("_")[1]) > int(currency_rates_file_name.split("_")[1]):
            currency_rates_file_name = file_name
    logger.info(f"Exchange rates file name: {currency_rates_file_name}")


def is_file_format_valid(file_lines) -> bool:
    """
    Function check whether the file lines are in correct format.
    File format is valid if the first line is header line, last line is trailer line,
    and in between are only detail record lines.
    Arguments:
        file_lines: File lines.
    Return: True if the file format is valid. Otherwise false.
    """

    if not file_lines:
        logger.error("No file lines")
        return False
    if (
        len([line for line in file_lines if line.startswith(cc.HEADER_DESCRIPTION)])
        != 1
    ):
        logger.error("Number of header lines in file is not 1")
        return False
    if (
        len([line for line in file_lines if line.startswith(cc.TRAILER_DESCRIPTION)])
        != 1
    ):
        logger.error("Number of trailer lines in file is not 1")
        return False
    if not file_lines[0].startswith(cc.HEADER_DESCRIPTION):
        logger.error("First line in the file is not the header")
        return False
    if not file_lines[-1].startswith(cc.TRAILER_DESCRIPTION):
        logger.error("Last line in the file is not the trailer")
        return False
    if len(
        [
            line
            for line in file_lines[1:-1]
            if not line.startswith(cc.DETAIL_DESCRIPTION)
        ]
    ):
        logger.error("Not all lines except the first and last one are detail records")
        return False

    return True


def is_data_record_allowed(data_record: DetailRecordModel) -> bool:
    """
    Function that check wheather the data record is allowed to be sent to API.
    Data record is allowed if its reference currency code is in the list of
    allowed reference codes defined in conversion constants,
    if its rate class is in the list of
    allowed rate classes defined in conversion constants,
    if its format indicator is in the list of
    allowed format indicators in conversion constants
    and its source currency code is in the list of
    allowed source codes defined in conversion constants.
    Arguments:
        data_record: Data record.
    Return: True if the data record is allowed. Otherwise False.
    """

    return (
        data_record != None
        and data_record.reference_currency_code in cc.ALLOWED_REFERENCE_CURRENCY_CODES
        and data_record.rate_class in cc.ALLOWED_RATE_CLASSES
        and data_record.rate_format_indicator in cc.ALLOWED_RATE_FORMAT_INDICATORS
        and data_record.source_currency_code not in cc.NOT_ALLOWED_SOURCE_CURRENCY_CODES
    )


def is_hash_valid(data: RequestModel) -> bool:
    """
    Function that checks weather the total hash is valid.
    Arguments:
        data: Data to be sent to API.
    Return: True if the total hash is valid.
    """

    # after conulting with Taryn, hash check is a good idea but a bit of a an overkill for this project
    return True


def convert_file_line_to_header(file_line: str) -> HeaderModel:
    """
    Function that converts file line to HeaderModel.
    Arguments:
        file_line: File line.
    Return: HeaderModel containing data from file line.
    """

    if not file_line or len(file_line) < cc.HEADER_FORMAT_VERSION_POSITION + 1:
        logger.error("Header length improper")
        return None
    elif (
        file_line[
            cc.HEADER_FORMAT_VERSION_POSITION : cc.HEADER_FORMAT_VERSION_POSITION + 1
        ]
        != cc.HEADER_FORMAT_VERSION
    ):
        logger.error("Header format unsupported")
        return None
    try:
        date = datetime.strptime(
            file_line[
                cc.HEADER_DATETIME_START_POSITION : cc.HEADER_DATETIME_END_POSITION + 1
            ],
            cc.HEADER_DATETIME_FORMAT,
        )
        return HeaderModel(date=str(date))
    except:
        logger.error("Header datetime improper format")
        return None


def convert_file_line_to_detail_record(file_line: str) -> DetailRecordModel:
    """
    Function that converts file line to DetailRecordModel.
    Arguments:
        file_line: File line.
    Return: DetailRecordModel containing data from file line.
    """

    if (
        file_line == None
        or len(file_line) < cc.SELL_CURRENCY_CONVERSION_RATE_END_POSSITION + 1
    ):
        logger.error("Detail record length improper")
        return None

    source_currency_number = file_line[
        cc.SOURCE_CURRENCY_START_POSSITION : cc.SOURCE_CURRENCY_END_POSSITION + 1
    ]
    reference_currency_code = file_line[
        cc.REFERENCE_CURRENCY_START_POSSITION : cc.REFERENCE_CURRENCY_END_POSSITION + 1
    ]
    source_currency_exponent = file_line[
        cc.SOURCE_CURRENCY_EXPONENT_START_POSSITION : cc.SOURCE_CURRENCY_EXPONENT_END_POSSITION
        + 1
    ]
    rate_class = file_line[
        cc.RATE_CLASS_START_POSSITION : cc.RATE_CLASS_END_POSSITION + 1
    ]
    rate_format_indicator = file_line[
        cc.RATE_FORMAT_INDICATOR_CLASS_START_POSSITION : cc.RATE_FORMAT_INDICATOR_CLASS_END_POSSITION
        + 1
    ]
    buy_currency_conversion_rate = file_line[
        cc.BUY_CURRENCY_CONVERSION_RATE_START_POSSITION : cc.BUY_CURRENCY_CONVERSION_RATE_END_POSSITION
        + 1
    ]
    mid_currency_conversion_rate = file_line[
        cc.MID_CURRENCY_CONVERSION_RATE_START_POSSITION : cc.MID_CURRENCY_CONVERSION_RATE_END_POSSITION
        + 1
    ]
    sell_currency_conversion_rate = file_line[
        cc.SELL_CURRENCY_CONVERSION_RATE_START_POSSITION : cc.SELL_CURRENCY_CONVERSION_RATE_END_POSSITION
        + 1
    ]

    try:
        record = DetailRecordModel(
            source_currency_number=int(source_currency_number),
            reference_currency_number=int(reference_currency_code),
            source_currency_exponent=int(source_currency_exponent),
            rate_class=rate_class,
            rate_format_indicator=rate_format_indicator,
            buy_currency_conversion_rate=convert_exchange_rate(
                buy_currency_conversion_rate
            ),
            mid_currency_conversion_rate=convert_exchange_rate(
                mid_currency_conversion_rate
            ),
            sell_currency_conversion_rate=convert_exchange_rate(
                sell_currency_conversion_rate
            ),
        )
        return record
    except:
        logger.error(f"Improper data record: {file_line}")
        return None


def convert_exchange_rate(conversion_rate: str) -> float:
    """
    Function that converts exchange rate as found in exchange
    rates file, to the float.
    Arguments:
        conversion_rate: Conversion rate to be converted to float.
    Return: Exchange rate as float.
    """

    # letting exceptions be caught by calling method
    if not conversion_rate or len(conversion_rate) != (
        cc.CURRENCY_COVERSION_INTEGER_PLACES + cc.CURRENCY_COVERSION_DECIMAL_PLACES
    ):
        logger.error("Conversion rate improper length")
        raise Exception()
    return float(
        conversion_rate[: cc.CURRENCY_COVERSION_INTEGER_PLACES]
        + "."
        + conversion_rate[cc.CURRENCY_COVERSION_INTEGER_PLACES :]
    )


def convert_file_line_to_trailer(file_line: str) -> TrailerModel:
    """
    Function that converts file line to TrailerModel.
    Arguments:
        file_line: File line.
    Return: TrailerModel containing data from file line.
    """

    if file_line == None or len(file_line) < cc.TRAILER_HASH_TOTAL_END_POSITION + 1:
        logger.error("Trailer length improper")
        return None
    try:
        return TrailerModel(
            total_records=int(
                file_line[
                    cc.TRAILER_TOTAL_RECORDS_START_POSITION : cc.TRAILER_TOTAL_RECORDS_END_POSITION
                    + 1
                ]
            ),
            hash_total=int(
                file_line[
                    cc.TRAILER_HASH_TOTAL_START_POSITION : cc.TRAILER_HASH_TOTAL_END_POSITION
                    + 1
                ]
            ),
        )
    except:
        logger.error("Trailer total records or hash improper format")
        return None


def main():
    import_rates()


if __name__ == "__main__":
    main()
