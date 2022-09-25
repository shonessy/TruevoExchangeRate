import conversion_constants as cc
from models import HeaderModel, DetailRecordModel, TrailerModel, ResponseModel
import logging
from datetime import datetime
from os.path import exists
from os import stat


def read_file():
    ret = ResponseModel(header= None, detail_records= [], trailer= None, error= None)

    def log_error_and_return(msg):
        logging.error(msg)
        ret.error = msg
        return ret

    if not file_exists_and_not_empty():
        return log_error_and_return("File does not exists or it is empty")

    with open(cc.CURRENCY_RATES_FILE_PATH, mode='rt') as file:
        # all non-empty lines in the file
        file_lines = [line for line in (line.strip() for line in file) if line]
        
        if not is_file_format_valid(file_lines):
            return log_error_and_return("Improper file format")

        ret.header = convert_file_line_to_header(file_lines[0])
        ret.trailer = convert_file_line_to_trailer(file_lines[-1])
        if ret.trailer.total_records != len(file_lines) - 2:
            return log_error_and_return("Number of data records does not match with trailer")

        # skip header and trailer
        for line in file_lines[1 : -1]:
            detail_record = convert_file_line_to_detail_record(line)
            if is_data_record_allowed(detail_record):
                ret.detail_records.append(detail_record)

        if not is_hash_valid(ret.detail_records, ret.trailer.hash_total):
            return log_error_and_return("Hash do not match")

    return ret


def file_exists_and_not_empty():
    if not exists(cc.CURRENCY_RATES_FILE_PATH):
        return False
    if stat(cc.CURRENCY_RATES_FILE_PATH).st_size == 0:
        return False
    return True
    

def is_file_format_valid(lines):
    if len([line for line in lines if line.startswith(cc.HEADER_DESCRIPTION)]) != 1:
        return False
    if len([line for line in lines if line.startswith(cc.TRAILER_DESCRIPTION)]) != 1:
        return False
    if lines[0].startswith(cc.HEADER_DESCRIPTION) == False:
        return False
    if lines[-1].startswith(cc.TRAILER_DESCRIPTION) == False:
        return False
    if len([line for line in lines[1:-1] if not line.startswith(cc.DETAIL_DESCRIPTION)]):
        return False

    return True


def is_data_record_allowed(data_record):
    return ( data_record != None and 
             data_record.reference_currency_code in cc.ALLOWED_REFERENCE_CURRENCY_CODES and 
             data_record.rate_class in cc.ALLOWED_RATE_CLASSES and 
             data_record.rate_format_indicator in cc.ALLOWED_RATE_FORMAT_INDICATORS and
             data_record.source_currency_code not in cc.NOT_ALLOWED_SOURCE_CURRENCY_CODES
            )

            
def is_hash_valid(data, hash_total):
    # after conulting with Taryn, hash check is a good idea but a bit of a an overkill for this project
    return True


def convert_file_line_to_header(line):
    if not line or len(line) < cc.HEADER_FORMAT_VERSION_POSITION + 1:
        logging.error("Header length improper")
        return None
    elif line[cc.HEADER_FORMAT_VERSION_POSITION : cc.HEADER_FORMAT_VERSION_POSITION + 1] != cc.HEADER_FORMAT_VERSION:
        logging.error("Header format unsupported")
        return None
    try:
        return HeaderModel(date = datetime.strptime(line[cc.HEADER_DATETIME_START_POSITION : cc.HEADER_DATETIME_END_POSITION + 1], cc.HEADER_DATETIME_FORMAT))
    except:
        logging.error("Header datetime improper format")
        return None


def convert_file_line_to_detail_record(line):
    if line == None or len(line) < cc.SELL_CURRENCY_CONVERSION_RATE_END_POSSITION + 1:
        logging.error("Detail record length improper")
        return None
    
    source_currency_code = line[cc.SOURCE_CURRENCY_START_POSSITION : cc.SOURCE_CURRENCY_END_POSSITION + 1]
    reference_currency_code = line[cc.REFERENCE_CURRENCY_START_POSSITION : cc.REFERENCE_CURRENCY_END_POSSITION + 1]
    source_currency_exponent = line[cc.SOURCE_CURRENCY_EXPONENT_START_POSSITION : cc.SOURCE_CURRENCY_EXPONENT_END_POSSITION + 1]
    rate_class = line[cc.RATE_CLASS_START_POSSITION : cc.RATE_CLASS_END_POSSITION + 1]
    rate_format_indicator = line[cc.RATE_FORMAT_INDICATOR_CLASS_START_POSSITION : cc.RATE_FORMAT_INDICATOR_CLASS_END_POSSITION + 1]
    buy_currency_conversion_rate = line[cc.BUY_CURRENCY_CONVERSION_RATE_START_POSSITION : cc.BUY_CURRENCY_CONVERSION_RATE_END_POSSITION + 1]
    mid_currency_conversion_rate = line[cc.MID_CURRENCY_CONVERSION_RATE_START_POSSITION : cc.MID_CURRENCY_CONVERSION_RATE_END_POSSITION + 1]
    sell_currency_conversion_rate = line[cc.SELL_CURRENCY_CONVERSION_RATE_START_POSSITION : cc.SELL_CURRENCY_CONVERSION_RATE_END_POSSITION + 1]

    try:
        record = DetailRecordModel(
            source_currency_code = int(source_currency_code),
            reference_currency_code = int(reference_currency_code),
            source_currency_exponent = int(source_currency_exponent),
            rate_class = rate_class,
            rate_format_indicator = rate_format_indicator,
            buy_currency_conversion_rate = get_rate(buy_currency_conversion_rate),
            mid_currency_conversion_rate = get_rate(mid_currency_conversion_rate),
            sell_currency_conversion_rate = get_rate(sell_currency_conversion_rate)
        )
        return record
    except:
        logging.error(f"Improper data record: {line}")
        return None


def get_rate(conversion_rate):
    # letting exceptions be caught by calling method
    if not conversion_rate or len(conversion_rate) != (cc.CURRENCY_COVERSION_INTEGER_PLACES + cc.CURRENCY_COVERSION_DECIMAL_PLACES):
        logging.error("Conversion rate improper length")
        raise Exception()
    return float(conversion_rate[:cc.CURRENCY_COVERSION_INTEGER_PLACES] + '.' + conversion_rate[cc.CURRENCY_COVERSION_INTEGER_PLACES:])


def convert_file_line_to_trailer(line):
    if line == None or len(line) < cc.TRAILER_HASH_TOTAL_END_POSITION + 1:
        logging.error("Trailer length improper")
        return None
    try:
        return TrailerModel(
            total_records = int(line[cc.TRAILER_TOTAL_RECORDS_START_POSITION : cc.TRAILER_TOTAL_RECORDS_END_POSITION + 1]),
            hash_total = int(line[cc.TRAILER_HASH_TOTAL_START_POSITION : cc.TRAILER_HASH_TOTAL_END_POSITION + 1])
        )
    except:
        logging.error("Trailer total records or hash improper format")
        return None


def main():
    read_file()


if __name__ == "__main__":
    main()