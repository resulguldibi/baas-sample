docker build -t resulguldibi/baas-sample-api baas-sample-api

docker-compose up -d

#create keyspace and table in cassandra. insert sample record to cassandra table.
docker exec -it cassandra cqlsh -e "CREATE KEYSPACE baas_keyspace WITH replication = {'class' : 'NetworkTopologyStrategy','my_datacenter' : 1};"

docker exec -it cassandra cqlsh -e "CREATE TABLE baas_keyspace.baas_logs (id text, tpp_code text, transaction_id text, status_code int, source text, controller text, action text, method text, query_string text, request text, response text, execution_time_ms bigint, message text, stack_trace text, insert_time bigint, insert_date int, PRIMARY KEY (id, tpp_code, transaction_id, source, controller, action, method, status_code, insert_date));"
docker exec -it cassandra cqlsh -e "CREATE INDEX on baas_keyspace.baas_logs(id);"

docker exec -it cassandra cqlsh -e "CREATE TABLE baas_keyspace.baas_rate_limit_definitions (tpp_code text, controller text, action text, method text, count bigint, PRIMARY KEY (tpp_code, controller, action, method));"
docker exec -it cassandra cqlsh -e "CREATE INDEX on baas_keyspace.baas_rate_limit_definitions(tpp_code);"

docker exec -it cassandra cqlsh -e "CREATE TABLE baas_keyspace.baas_rate_limit_transactions (id text, tpp_code text, controller text, action text, method text, status_code int,insert_time bigint, insert_date int, PRIMARY KEY (id, tpp_code, controller, action, method, status_code, insert_date));"
docker exec -it cassandra cqlsh -e "CREATE INDEX on baas_keyspace.baas_rate_limit_transactions(tpp_code);"