
--create test-topic in kafka
./kafka-topics.sh --zookeeper 172.17.0.1:2181 --topic test-topic --partitions 2 --replication-factor 1 --create