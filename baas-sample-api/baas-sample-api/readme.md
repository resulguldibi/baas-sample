send logs to kafka and consumes message kafka and persist them (to database/elastic search)
check idempotemcy from redis or cassandra/oracle
check rate limiting from redis or cassandra/oracle
check data integrity from redis or cassandra/oracle
check authorization from cassandra/oracle


#idempotemcy

#rate limit

determine rate limit definitions

baas_rate_limit_definitions

id tpp application controller action method limit_type limit_count status


baas_rate_limit_transactions

id tpp application controller action method transaction_time insert_time status_code

#quota management

baas_quota_definitions

id tpp application controller action method quota_key_source_type quota_key_source_name quota_count status


baas_quota_transactions

id tpp application controller action method quota_key_source_type quota_key_source_name quota_key_source_value transaction_time insert_time status_code






