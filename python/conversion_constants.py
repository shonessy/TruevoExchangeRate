CURRENCY_RATES_FILE_PATH = 'I_171021_T057.sw0'

HEADER_DESCRIPTION = 'H'
HEADER_DESCRIPTION_START_POSITION = 0
HEADER_DESCRIPTION_END_POSITION = 0

HEADER_DATETIME_START_POSITION = 1
HEADER_DATETIME_END_POSITION = 14
HEADER_DATETIME_FORMAT = '%Y%m%d%H%M%S'

HEADER_FORMAT_VERSION = '1'
HEADER_FORMAT_VERSION_POSITION = 15

DETAIL_DESCRIPTION = 'D'
DETAIL_DESCRIPTION_START_POSITION = 0
DETAIL_DESCRIPTION_END_POSITION = 0

SOURCE_CURRENCY_START_POSSITION = 1
SOURCE_CURRENCY_END_POSSITION = 3

REFERENCE_CURRENCY_START_POSSITION = 4
REFERENCE_CURRENCY_END_POSSITION = 6

SOURCE_CURRENCY_EXPONENT_START_POSSITION = 7
SOURCE_CURRENCY_EXPONENT_END_POSSITION = 7

RATE_CLASS_START_POSSITION = 8
RATE_CLASS_END_POSSITION = 8

RATE_FORMAT_INDICATOR_CLASS_START_POSSITION = 9
RATE_FORMAT_INDICATOR_CLASS_END_POSSITION = 9

BUY_CURRENCY_CONVERSION_RATE_START_POSSITION = 10
BUY_CURRENCY_CONVERSION_RATE_END_POSSITION = 24

MID_CURRENCY_CONVERSION_RATE_START_POSSITION = 25
MID_CURRENCY_CONVERSION_RATE_END_POSSITION = 39

SELL_CURRENCY_CONVERSION_RATE_START_POSSITION = 40
SELL_CURRENCY_CONVERSION_RATE_END_POSSITION = 54

CURRENCY_COVERSION_INTEGER_PLACES = 7
CURRENCY_COVERSION_DECIMAL_PLACES = 8

TRAILER_DESCRIPTION = 'T'
TRAILER_DESCRIPTION_START_POSITION = 0
TRAILER_DESCRIPTION_END_POSITION = 0

TRAILER_TOTAL_RECORDS_START_POSITION = 1
TRAILER_TOTAL_RECORDS_END_POSITION = 6

TRAILER_HASH_TOTAL_START_POSITION = 7
TRAILER_HASH_TOTAL_END_POSITION = 23

ALLOWED_REFERENCE_CURRENCY_CODES = [840,]
ALLOWED_RATE_CLASSES = ["M"]
ALLOWED_RATE_FORMAT_INDICATORS = ["D"]
NOT_ALLOWED_SOURCE_CURRENCY_CODES = [157, 158, 222, 996]

