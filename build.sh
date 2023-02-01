docker build -t resulguldibi/baas-sample-api baas-sample-api

docker-compose up -d


docker exec -it cassandra cqlsh -e "DROP KEYSPACE baas_keyspace;"

#create keyspace and table in cassandra. insert sample record to cassandra table.
docker exec -it cassandra cqlsh -e "CREATE KEYSPACE baas_keyspace WITH replication = {'class' : 'NetworkTopologyStrategy','my_datacenter' : 1};"

docker exec -it cassandra cqlsh -e "CREATE TABLE baas_keyspace.baas_logs (id text, tpp_code text, transaction_id text, status_code int, source text, controller text, action text, method text, query_string text, request text, response text, execution_time_ms bigint, message text, stack_trace text, insert_time bigint, insert_date int, PRIMARY KEY (id, tpp_code, transaction_id, source, controller, action, method, status_code, insert_date));"

docker exec -it cassandra cqlsh -e "CREATE TABLE baas_keyspace.baas_rate_limit_definitions (id uuid, tpp_code text, application text, controller text, action text, method text, limit_period bigint, limit_count bigint, status boolean, PRIMARY KEY ((tpp_code, application, controller, action, method, status), id));"
docker exec -it cassandra cqlsh -e "CREATE INDEX on baas_keyspace.baas_rate_limit_definitions(id);"
docker exec -it cassandra cqlsh -e "INSERT INTO baas_keyspace.baas_rate_limit_definitions(id,tpp_code, application, controller, action, method, limit_period, limit_count, status) values(uuid(), 'tpp-1','baas-sample','WeatherForecast','items','POST',3600,5, true);"

docker exec -it cassandra cqlsh -e "CREATE TABLE baas_keyspace.baas_rate_limit_transactions (id uuid, definition_id uuid, status_code int, insert_time bigint, transaction_time bigint, PRIMARY KEY (definition_id, transaction_time, id));"
docker exec -it cassandra cqlsh -e "CREATE INDEX on baas_keyspace.baas_rate_limit_transactions(id);"

docker exec -it cassandra cqlsh -e "CREATE TABLE baas_keyspace.baas_quota_definitions (id uuid, tpp_code text, application text, controller text, action text, method text, quota_period bigint, quota_key_source_type text,quota_key_source_name text, quota_count bigint, status boolean, PRIMARY KEY ((tpp_code, application, controller, action, method, status), id));"
docker exec -it cassandra cqlsh -e "CREATE INDEX on baas_keyspace.baas_quota_definitions(id);"
docker exec -it cassandra cqlsh -e "INSERT INTO baas_keyspace.baas_quota_definitions(id,tpp_code, application, controller, action, method, quota_period, quota_key_source_type, quota_key_source_name, quota_count, status) values(uuid(), 'tpp-1','baas-sample','WeatherForecast','quota_with_request_header','GET', 3600, 'header','x-tpp-code',5, true);"
docker exec -it cassandra cqlsh -e "INSERT INTO baas_keyspace.baas_quota_definitions(id,tpp_code, application, controller, action, method, quota_period, quota_key_source_type, quota_key_source_name, quota_count, status) values(uuid(), 'tpp-1','baas-sample','WeatherForecast','quota_with_request_body_json_path','POST', 3600, 'body','date',5, true);"
docker exec -it cassandra cqlsh -e "INSERT INTO baas_keyspace.baas_quota_definitions(id,tpp_code, application, controller, action, method, quota_period, quota_key_source_type, quota_key_source_name, quota_count, status) values(uuid(), 'tpp-1','baas-sample','WeatherForecast','quota_with_query_string','GET', 3600, 'query','name',5, true);"
docker exec -it cassandra cqlsh -e "INSERT INTO baas_keyspace.baas_quota_definitions(id,tpp_code, application, controller, action, method, quota_period, quota_key_source_type, quota_key_source_name, quota_count, status) values(uuid(), 'tpp-1','baas-sample','WeatherForecast','quota_with_route/{id}','GET', 3600, 'route','id',5, true);"

docker exec -it cassandra cqlsh -e "CREATE TABLE baas_keyspace.baas_quota_transactions (id uuid, definition_id uuid, status_code int, quota_key_source_value text, insert_time bigint, transaction_time bigint, PRIMARY KEY (definition_id, quota_key_source_value, transaction_time, id));"
docker exec -it cassandra cqlsh -e "CREATE INDEX on baas_keyspace.baas_quota_transactions(id);"

docker exec -it cassandra cqlsh -e "CREATE TABLE baas_keyspace.baas_consent_definitions (id uuid, tpp_code text, application text, controller text, action text, method text, consent_key_source_type text, consent_key_source_name text, status boolean, PRIMARY KEY ((tpp_code, application, controller, action, method), id));"
docker exec -it cassandra cqlsh -e "CREATE INDEX on baas_keyspace.baas_consent_definitions(id);"


docker exec -it cassandra cqlsh -e "CREATE TABLE baas_keyspace.baas_consent_data (id uuid, definition_id uuid, customer_id bigint, consent_key_source_value text, consent_expiration_time bigint, consent_cancellation_time bigint, status boolean, PRIMARY KEY ((definition_id), id));"
docker exec -it cassandra cqlsh -e "CREATE INDEX on baas_keyspace.baas_consent_data(id);"

docker exec -it kafka bash -c "kafka-topics.sh --zookeeper zookeeper:2181 --topic baas_logs --partitions 1 --replication-factor 1 --create"