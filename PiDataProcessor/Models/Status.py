from enum import Enum

class Status(Enum):
    '''Alert Level Status'''
    LOW = 'HEALTHY'
    MEDIUM = 'UNHEALTHY'
    HIGH = 'DANGER'
